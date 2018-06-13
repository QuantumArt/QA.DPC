import "reflect-metadata";
import React from "react";
import { render } from "react-dom";
import { types as t, unprotect, IExtendedObservableMap } from "mobx-state-tree";
import { IObservableArray } from "mobx";
import { ValidatableControl } from "Components/FormControls/AbstractControls";
import { validatableMixin } from "Models/ValidatableMixin";
import { required, pattern, maxCount } from "Utils/Validators";

class TestControl extends ValidatableControl {
  render() {
    return <div />;
  }
}

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

describe("ValidatableMixin", () => {
  it("should validate plain field", async () => {
    const Article = t
      .model("Article", {
        Id: t.identifier(t.number),
        Title: t.maybe(t.string)
      })
      .extend(validatableMixin);

    const article = Article.create({
      Id: 123,
      Title: "foo"
    });

    unprotect(article);

    render(
      <TestControl name="Title" model={article} validate={[required, pattern(/^[A-Z]+$/i)]} />,
      document.createElement("div")
    );

    expect(article.hasErrors("Title")).toBeFalsy();

    article.Title = "123";
    await delay(2);
    expect(article.getErrors("Title")).toEqual(["Поле не соответствует шаблону"]);

    article.Title = "123";
    await delay(2);
    expect(article.getErrors("Title")).toEqual(["Поле не соответствует шаблону"]);

    article.Title = null;
    await delay(2);
    expect(article.getErrors("Title")).toEqual(["Поле обязательно для заполнения"]);

    article.Title = null;
    await delay(2);
    expect(article.getErrors("Title")).toEqual(["Поле обязательно для заполнения"]);

    article.Title = "foo";
    await delay(2);
    expect(article.hasErrors("Title")).toBeFalsy();
  });

  it("should validate model field", async () => {
    const Article = t
      .model("Article", {
        Id: t.identifier(t.number),
        Category: t.maybe(
          t.model({
            Name: t.maybe(t.string)
          })
        )
      })
      .extend(validatableMixin);

    const article = Article.create({ Id: 123 });

    unprotect(article);

    render(
      <TestControl name="Category" model={article} validate={required} />,
      document.createElement("div")
    );

    expect(article.getErrors("Category")).toEqual(["Поле обязательно для заполнения"]);

    article.Category = null;
    await delay(2);
    expect(article.getErrors("Category")).toEqual(["Поле обязательно для заполнения"]);

    article.Category = { Name: "bar" };
    await delay(2);
    expect(article.hasErrors("Category")).toBeFalsy();

    article.Category = { Name: "bar" };
    await delay(2);
    expect(article.hasErrors("Category")).toBeFalsy();
  });

  it("should validate reference field", async () => {
    const Category = t
      .model("Category", {
        Id: t.identifier(t.number),
        Name: t.maybe(t.string)
      })
      .extend(validatableMixin);

    const Article = t
      .model("Article", {
        Id: t.identifier(t.number),
        Category: t.maybe(t.reference(Category))
      })
      .extend(validatableMixin);

    const category = Category.create({
      Id: 4567,
      Name: "bar"
    });

    unprotect(category);

    const article = Article.create({ Id: 123 });

    unprotect(article);

    render(
      <TestControl name="Category" model={article} validate={required} />,
      document.createElement("div")
    );

    expect(article.getErrors("Category")).toEqual(["Поле обязательно для заполнения"]);

    article.Category = null;
    await delay(2);
    expect(article.getErrors("Category")).toEqual(["Поле обязательно для заполнения"]);

    article.Category = category;
    await delay(2);
    expect(article.hasErrors("Category")).toBeFalsy();

    article.Category = category;
    await delay(2);
    expect(article.hasErrors("Category")).toBeFalsy();

    article.Category = Category.create({ Id: 5678, Name: "foo" });
    await delay(2);
    expect(article.hasErrors("Category")).toBeFalsy();
  });

  it("should validate array field", async () => {
    const Article = t
      .model("Article", {
        Id: t.identifier(t.number),
        Tags: t.maybe(t.array(t.string))
      })
      .extend(validatableMixin);

    const article = Article.create({
      Id: 123,
      Tags: ["first"]
    });

    unprotect(article);

    render(
      <TestControl name="Tags" model={article} validate={[required, maxCount(1)]} />,
      document.createElement("div")
    );

    expect(article.hasErrors("Tags")).toBeFalsy();

    article.Tags.push("second");
    await delay(2);
    expect(article.getErrors("Tags")).toEqual(["Допустимо не более 1 элементов"]);

    article.Tags = null;
    await delay(2);
    expect(article.getErrors("Tags")).toEqual(["Поле обязательно для заполнения"]);

    article.Tags = null;
    await delay(2);
    expect(article.getErrors("Tags")).toEqual(["Поле обязательно для заполнения"]);

    article.Tags = ["first", "second"] as IObservableArray<string>;
    await delay(2);
    expect(article.getErrors("Tags")).toEqual(["Допустимо не более 1 элементов"]);

    article.Tags.pop();
    await delay(2);
    expect(article.hasErrors("Tags")).toBeFalsy();

    article.Tags = ["first", "second"] as IObservableArray<string>;
    await delay(2);
    expect(article.getErrors("Tags")).toEqual(["Допустимо не более 1 элементов"]);

    article.Tags = ["first", "second"] as IObservableArray<string>;
    await delay(2);
    expect(article.getErrors("Tags")).toEqual(["Допустимо не более 1 элементов"]);

    article.Tags[1] = "first";
    await delay(2);
    expect(article.getErrors("Tags")).toEqual(["Допустимо не более 1 элементов"]);
  });

  it("should validate map field", async () => {
    const Comment = t.model("Comment", {
      Id: t.identifier(t.number)
    });

    const Article = t
      .model("Article", {
        Id: t.identifier(t.number),
        Comments: t.maybe(t.map(Comment)),
        Comments2: t.optional(t.array(t.reference(t.reference(Comment))), [])
      })
      .extend(validatableMixin);

    const article = Article.create({
      Id: 123,
      Comments: {}
    });

    unprotect(article);

    const shouldHaveComments = (value: IExtendedObservableMap<Comment>) =>
      value && value.keys().next().done ? "Статья должна иметь комментарии" : undefined;

    const maxComments = (max: number) => (value: IExtendedObservableMap<Comment>) =>
      value && Array.from(value.keys()).length > max
        ? `Допустимо не более ${max} комментариев`
        : undefined;

    render(
      <TestControl
        name="Comments"
        model={article}
        validate={[required, shouldHaveComments, maxComments(1)]}
      />,
      document.createElement("div")
    );

    expect(article.getErrors("Comments")).toEqual(["Статья должна иметь комментарии"]);

    article.Comments = null;
    await delay(2);
    expect(article.getErrors("Comments")).toEqual(["Поле обязательно для заполнения"]);

    article.Comments = null;
    await delay(2);
    expect(article.getErrors("Comments")).toEqual(["Поле обязательно для заполнения"]);

    article.Comments = {} as any;
    await delay(2);
    expect(article.getErrors("Comments")).toEqual(["Статья должна иметь комментарии"]);

    article.Comments = {} as any;
    await delay(2);
    expect(article.getErrors("Comments")).toEqual(["Статья должна иметь комментарии"]);

    article.Comments.put({ Id: 4567 });
    article.Comments.delete(String(5678));
    await delay(2);
    expect(article.hasErrors("Comments")).toBeFalsy();

    article.Comments.delete(String(4567));
    await delay(2);
    expect(article.getErrors("Comments")).toEqual(["Статья должна иметь комментарии"]);

    article.Comments.put({ Id: 4567 });
    article.Comments.put({ Id: 5678 });
    await delay(2);
    expect(article.getErrors("Comments")).toEqual(["Допустимо не более 1 комментариев"]);

    article.Comments.put({ Id: 4567 });
    article.Comments.put({ Id: 5678 });
    await delay(2);
    expect(article.getErrors("Comments")).toEqual(["Допустимо не более 1 комментариев"]);
  });
});
