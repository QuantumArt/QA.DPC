export class Localization {
  constructor(settings: object, stringsKey: string = "strings") {
    if (settings[stringsKey]) {
      this.strings = settings[stringsKey];
    } else {
      console.error("No strings found in window");
    }
  }
  public get = (key: string) => {
    if (this.strings[key]) {
      return this.strings[key];
    } else {
      console.log("No such string");
      return "@";
    }
  };

  private readonly strings;
}
