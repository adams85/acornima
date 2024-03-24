class X {
  static #privateField = 'super';
  static #getPrivateField() {
    return X.#privateField;
  }
}
