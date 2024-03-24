class aa {
  static accessor = true;
  accessor = true;
  #accessor = true;
}
class bb {
  static accessor() { };
  accessor() { }
  #accessor() { };
}
class cc {
  static async accessor() { };
  async accessor() { }
  async #accessor() { };
}
