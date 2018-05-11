/**
 * Заменить объекты c одинаковым Id на перевое вхождение такого объекта
 * @param article
 */
export function deduplicateArticles(article: Object) {
  const articlesById: { [id: number]: Object } = Object.create(null);

  visit(article);

  return articlesById;

  function visit(arg: any) {
    if (Array.isArray(arg)) {
      arg.forEach((el, i) => {
        if (isObject(el) && isInteger(el.Id)) {
          const article = articlesById[el.Id];
          if (article) {
            arg[i] = article;
          } else {
            articlesById[el.Id] = el;
            visit(el);
          }
        } else {
          visit(el);
        }
      });
    } else if (isObject(arg)) {
      for (let key in arg) {
        const val = arg[key];
        if (isObject(val) && isInteger(val.Id)) {
          const article = articlesById[val.Id];
          if (article) {
            arg[key] = article;
          } else {
            articlesById[val.Id] = val;
            visit(val);
          }
        } else {
          visit(val);
        }
      }
    }
  }
}

function isInteger(arg): arg is number {
  return typeof arg === "number" && /^-?[0-9]+$/.test(String(arg));
}

function isObject(arg: any): arg is Object {
  return typeof arg === "object" && arg !== null && !Array.isArray(arg);
}
