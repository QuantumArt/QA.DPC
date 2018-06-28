import React from "react";

export default {
  "Test...": `Тест...`,
  "Hello, ${name}!": name => `Привет, ${name}!`,
  helloTemplate: name => `Здравствуй, ${name}!`,
  customComponent: ({ firstName, lastName, fullName }) => (
    <article key={1}>
      Карточка пользователя
      <div key={2}>Имя: {firstName}</div>
      <div key={3}>Фамилия: {lastName}</div>
      <div key={4}>Полное имя: {fullName}</div>
    </article>
  )
};
