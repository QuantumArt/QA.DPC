const urlFromHead = document.head.getAttribute("root-url") || "";
export const rootUrl = urlFromHead.endsWith("/") ? urlFromHead.slice(0, -1) : urlFromHead;

export function newUid() {
  return Math.random()
    .toString(36)
    .slice(2);
}

const normailzedStrings: { [key: string]: string } = Object.create(null);

export function normailzeSearchString(input: string) {
  return (
    normailzedStrings[input] ||
    (normailzedStrings[input] = input
      .toLowerCase()
      .replace(/[^а-яё]+/g, " ")
      .trim())
  );
}
