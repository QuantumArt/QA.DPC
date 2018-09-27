export class WeakCache<TKey extends object = object, TValue = any> {
  private _weakMap = new WeakMap<object, any>();

  public get<V extends TValue>(key: TKey): V | undefined {
    return this._weakMap.get(key);
  }

  public set(key: TKey, value: TValue): this {
    this._weakMap.set(key, value);
    return this;
  }

  public getOrAdd<K extends TKey, V extends TValue>(key: K, getValue: (key: K) => V): V {
    let value = this._weakMap.get(key);
    if (!value) {
      value = getValue(key);
      this._weakMap.set(key, value);
    }
    return value;
  }
}
