import template from './template.mjs';

const data = {
  msg: "Hello world!",
  attrs: { "class": "hello" }
};

const element = template(data);

console.log(element.render(true));