export function newUid() {
  return Math.random()
    .toString(36)
    .slice(2);
}
