var urlFromHead = document.head.getAttribute("root-url") || "";
export var rootUrl = urlFromHead.endsWith("/")
  ? urlFromHead.slice(0, -1)
  : urlFromHead;
export function newUid() {
  return Math.random()
    .toString(36)
    .slice(2);
}
//# sourceMappingURL=Common.js.map
