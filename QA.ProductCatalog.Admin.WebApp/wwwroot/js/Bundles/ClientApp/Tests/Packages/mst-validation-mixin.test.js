var _this = this;
import * as tslib_1 from "tslib";
import React from "react";
import { render } from "react-dom";
import { types as t, unprotect } from "mobx-state-tree";
import { validationMixin, Validate } from "mst-validation-mixin";
import { required, pattern, maxCount } from "Utils/Validators";
var delay = function(ms) {
  return new Promise(function(resolve) {
    return setTimeout(resolve, ms);
  });
};
describe("mst-validation-mixin", function() {
  it("should validate plain field", function() {
    return tslib_1.__awaiter(_this, void 0, void 0, function() {
      var Article, article;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            Article = t
              .model("Article", {
                Id: t.identifierNumber,
                Title: t.maybeNull(t.string)
              })
              .extend(validationMixin);
            article = Article.create({
              Id: 123,
              Title: "foo"
            });
            unprotect(article);
            render(
              React.createElement(Validate, {
                name: "Title",
                model: article,
                rules: [required, pattern(/^[A-Z]+$/i)]
              }),
              document.createElement("div")
            );
            expect(article.hasErrors("Title")).toBeFalsy();
            article.Title = "123";
            return [4 /*yield*/, delay(2)];
          case 1:
            _a.sent();
            expect(article.getErrors("Title")).toEqual([
              "Поле не соответствует шаблону"
            ]);
            article.Title = "123";
            return [4 /*yield*/, delay(2)];
          case 2:
            _a.sent();
            expect(article.getErrors("Title")).toEqual([
              "Поле не соответствует шаблону"
            ]);
            article.Title = null;
            return [4 /*yield*/, delay(2)];
          case 3:
            _a.sent();
            expect(article.getErrors("Title")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Title = null;
            return [4 /*yield*/, delay(2)];
          case 4:
            _a.sent();
            expect(article.getErrors("Title")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Title = "foo";
            return [4 /*yield*/, delay(2)];
          case 5:
            _a.sent();
            expect(article.hasErrors("Title")).toBeFalsy();
            return [2 /*return*/];
        }
      });
    });
  });
  it("should validate model field", function() {
    return tslib_1.__awaiter(_this, void 0, void 0, function() {
      var Article, article;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            Article = t
              .model("Article", {
                Id: t.identifierNumber,
                Category: t.maybeNull(
                  t.model({
                    Name: t.maybeNull(t.string)
                  })
                )
              })
              .extend(validationMixin);
            article = Article.create({ Id: 123 });
            unprotect(article);
            render(
              React.createElement(Validate, {
                name: "Category",
                model: article,
                rules: required
              }),
              document.createElement("div")
            );
            expect(article.getErrors("Category")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Category = null;
            return [4 /*yield*/, delay(2)];
          case 1:
            _a.sent();
            expect(article.getErrors("Category")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Category = { Name: "bar" };
            return [4 /*yield*/, delay(2)];
          case 2:
            _a.sent();
            expect(article.hasErrors("Category")).toBeFalsy();
            article.Category = { Name: "bar" };
            return [4 /*yield*/, delay(2)];
          case 3:
            _a.sent();
            expect(article.hasErrors("Category")).toBeFalsy();
            return [2 /*return*/];
        }
      });
    });
  });
  it("should validate reference field", function() {
    return tslib_1.__awaiter(_this, void 0, void 0, function() {
      var Category, Article, category, article;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            Category = t
              .model("Category", {
                Id: t.identifierNumber,
                Name: t.maybeNull(t.string)
              })
              .extend(validationMixin);
            Article = t
              .model("Article", {
                Id: t.identifierNumber,
                Category: t.maybeNull(t.reference(Category))
              })
              .extend(validationMixin);
            category = Category.create({
              Id: 4567,
              Name: "bar"
            });
            unprotect(category);
            article = Article.create({ Id: 123 });
            unprotect(article);
            render(
              React.createElement(Validate, {
                name: "Category",
                model: article,
                rules: required
              }),
              document.createElement("div")
            );
            expect(article.getErrors("Category")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Category = null;
            return [4 /*yield*/, delay(2)];
          case 1:
            _a.sent();
            expect(article.getErrors("Category")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Category = category;
            return [4 /*yield*/, delay(2)];
          case 2:
            _a.sent();
            expect(article.hasErrors("Category")).toBeFalsy();
            article.Category = category;
            return [4 /*yield*/, delay(2)];
          case 3:
            _a.sent();
            expect(article.hasErrors("Category")).toBeFalsy();
            article.Category = Category.create({ Id: 5678, Name: "foo" });
            return [4 /*yield*/, delay(2)];
          case 4:
            _a.sent();
            expect(article.hasErrors("Category")).toBeFalsy();
            return [2 /*return*/];
        }
      });
    });
  });
  it("should validate array field", function() {
    return tslib_1.__awaiter(_this, void 0, void 0, function() {
      var Article, article;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            Article = t
              .model("Article", {
                Id: t.identifierNumber,
                Tags: t.maybeNull(t.array(t.string))
              })
              .extend(validationMixin);
            article = Article.create({
              Id: 123,
              Tags: ["first"]
            });
            unprotect(article);
            render(
              React.createElement(Validate, {
                name: "Tags",
                model: article,
                rules: [required, maxCount(1)]
              }),
              document.createElement("div")
            );
            expect(article.hasErrors("Tags")).toBeFalsy();
            article.Tags.push("second");
            return [4 /*yield*/, delay(2)];
          case 1:
            _a.sent();
            expect(article.getErrors("Tags")).toEqual([
              "Допустимо не более 1 элементов"
            ]);
            article.Tags = null;
            return [4 /*yield*/, delay(2)];
          case 2:
            _a.sent();
            expect(article.getErrors("Tags")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Tags = null;
            return [4 /*yield*/, delay(2)];
          case 3:
            _a.sent();
            expect(article.getErrors("Tags")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Tags = ["first", "second"];
            return [4 /*yield*/, delay(2)];
          case 4:
            _a.sent();
            expect(article.getErrors("Tags")).toEqual([
              "Допустимо не более 1 элементов"
            ]);
            article.Tags.pop();
            return [4 /*yield*/, delay(2)];
          case 5:
            _a.sent();
            expect(article.hasErrors("Tags")).toBeFalsy();
            article.Tags = ["first", "second"];
            return [4 /*yield*/, delay(2)];
          case 6:
            _a.sent();
            expect(article.getErrors("Tags")).toEqual([
              "Допустимо не более 1 элементов"
            ]);
            article.Tags = ["first", "second"];
            return [4 /*yield*/, delay(2)];
          case 7:
            _a.sent();
            expect(article.getErrors("Tags")).toEqual([
              "Допустимо не более 1 элементов"
            ]);
            article.Tags[1] = "first";
            return [4 /*yield*/, delay(2)];
          case 8:
            _a.sent();
            expect(article.getErrors("Tags")).toEqual([
              "Допустимо не более 1 элементов"
            ]);
            return [2 /*return*/];
        }
      });
    });
  });
  it("should validate map field", function() {
    return tslib_1.__awaiter(_this, void 0, void 0, function() {
      var Comment, Article, article, shouldHaveComments, maxComments;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            Comment = t.model("Comment", {
              Id: t.identifierNumber
            });
            Article = t
              .model("Article", {
                Id: t.identifierNumber,
                Comments: t.maybeNull(t.map(Comment))
              })
              .extend(validationMixin);
            article = Article.create({
              Id: 123,
              Comments: {}
            });
            unprotect(article);
            shouldHaveComments = function(value) {
              return value && value.keys().next().done
                ? "Статья должна иметь комментарии"
                : undefined;
            };
            maxComments = function(max) {
              return function(value) {
                return value && Array.from(value.keys()).length > max
                  ? "\u0414\u043E\u043F\u0443\u0441\u0442\u0438\u043C\u043E \u043D\u0435 \u0431\u043E\u043B\u0435\u0435 " +
                      max +
                      " \u043A\u043E\u043C\u043C\u0435\u043D\u0442\u0430\u0440\u0438\u0435\u0432"
                  : undefined;
              };
            };
            render(
              React.createElement(Validate, {
                name: "Comments",
                model: article,
                rules: [required, shouldHaveComments, maxComments(1)]
              }),
              document.createElement("div")
            );
            expect(article.getErrors("Comments")).toEqual([
              "Статья должна иметь комментарии"
            ]);
            article.Comments = null;
            return [4 /*yield*/, delay(2)];
          case 1:
            _a.sent();
            expect(article.getErrors("Comments")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Comments = null;
            return [4 /*yield*/, delay(2)];
          case 2:
            _a.sent();
            expect(article.getErrors("Comments")).toEqual([
              "Поле обязательно для заполнения"
            ]);
            article.Comments = {};
            return [4 /*yield*/, delay(2)];
          case 3:
            _a.sent();
            expect(article.getErrors("Comments")).toEqual([
              "Статья должна иметь комментарии"
            ]);
            article.Comments = {};
            return [4 /*yield*/, delay(2)];
          case 4:
            _a.sent();
            expect(article.getErrors("Comments")).toEqual([
              "Статья должна иметь комментарии"
            ]);
            article.Comments.put({ Id: 4567 });
            article.Comments.delete(String(5678));
            return [4 /*yield*/, delay(2)];
          case 5:
            _a.sent();
            expect(article.hasErrors("Comments")).toBeFalsy();
            article.Comments.delete(String(4567));
            return [4 /*yield*/, delay(2)];
          case 6:
            _a.sent();
            expect(article.getErrors("Comments")).toEqual([
              "Статья должна иметь комментарии"
            ]);
            article.Comments.put({ Id: 4567 });
            article.Comments.put({ Id: 5678 });
            return [4 /*yield*/, delay(2)];
          case 7:
            _a.sent();
            expect(article.getErrors("Comments")).toEqual([
              "Допустимо не более 1 комментариев"
            ]);
            article.Comments.put({ Id: 4567 });
            article.Comments.put({ Id: 5678 });
            return [4 /*yield*/, delay(2)];
          case 8:
            _a.sent();
            expect(article.getErrors("Comments")).toEqual([
              "Допустимо не более 1 комментариев"
            ]);
            return [2 /*return*/];
        }
      });
    });
  });
});
//# sourceMappingURL=mst-validation-mixin.test.js.map
