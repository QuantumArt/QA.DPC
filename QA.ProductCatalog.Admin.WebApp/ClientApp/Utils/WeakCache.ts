export class WeakCache<TK extends object = object, TV = any> {
  private _weakMap = new WeakMap<object, any>();

  public get<V extends TV>(key: TK): V | undefined {
    return this._weakMap.get(key);
  }

  public set(key: TK, value: TV): this {
    this._weakMap.set(key, value);
    return this;
  }

  public getOrAdd<K extends TK, V extends TV>(key: K, getValue: (key: K) => V): V {
    let value = this._weakMap.get(key);
    if (!value) {
      value = getValue(key);
      this._weakMap.set(key, value);
    }
    return value;
  }
}
