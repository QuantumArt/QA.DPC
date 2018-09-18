import "reflect-metadata";
import React from "react";
import { render } from "react-dom";
import { types as t, unprotect, IMSTMap } from "mobx-state-tree";
import { IObservableArray } from "mobx";
import { validationMixin, Validate } from "mst-validation-mixin";
import { required, pattern, maxCount } from "Utils/Validators";

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

describe("mst-validation-mixin", () => {
  it("should validate plain field", async () => {
    const Article = t
      .model("Article", {
        Id: t.identifierNumber,
        Title: t.maybeNull(t.string)
      })
      .extend(validationMixin);

    const article = Article.create({
      Id: 123,
      Title: "foo"
    });

    unprotect(article);

    render(
      <Validate name="Title" model={article} rules={[required, pattern(/^[A-Z]+$/i)]} />,
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
        Id: t.identifierNumber,
        Category: t.maybeNull(
          t.model({
            Name: t.maybeNull(t.string)
          })
        )
      })
      .extend(validationMixin);

    const article = Article.create({ Id: 123 });

    unprotect(article);

    render(
      <Validate name="Category" model={article} rules={required} />,
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
        Id: t.identifierNumber,
        Name: t.maybeNull(t.string)
      })
      .extend(validationMixin);

    const Article = t
      .model("Article", {
        Id: t.identifierNumber,
        Category: t.maybeNull(t.reference(Category))
      })
      .extend(validationMixin);

    const category = Category.create({
      Id: 4567,
      Name: "bar"
    });

    unprotect(category);

    const article = Article.create({ Id: 123 });

    unprotect(article);

    render(
      <Validate name="Category" model={article} rules={required} />,
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
        Id: t.identifierNumber,
        Tags: t.maybeNull(t.array(t.string))
      })
      .extend(validationMixin);

    const article = Article.create({
      Id: 123,
      Tags: ["first"]
    });

    unprotect(article);

    render(
      <Validate name="Tags" model={article} rules={[required, maxCount(1)]} />,
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
      Id: t.identifierNumber
    });

    const Article = t
      .model("Article", {
        Id: t.identifierNumber,
        Comments: t.maybeNull(t.map(Comment))
      })
      .extend(validationMixin);

    const article = Article.create({
      Id: 123,
      Comments: {}
    });

    unprotect(article);

    const shouldHaveComments = (value: IMSTMap<any, any, Comment>) =>
      value && value.keys().next().done ? "Статья должна иметь комментарии" : undefined;

    const maxComments = (max: number) => (value: IMSTMap<any, any, Comment>) =>
      value && Array.from(value.keys()).length > max
        ? `Допустимо не более ${max} комментариев`
        : undefined;

    render(
      <Validate
        name="Comments"
        model={article}
        rules={[required, shouldHaveComments, maxComments(1)]}
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
