import React from "react";

export default {
  "User Card": `Карточка пользователя`,
  "First Name": `Имя`,
  "Last Name": `Фамилия`,
  "Full Name": `Полное имя`,
  "Test...": `Тест...`,
  "Hello, ${name}!": name => `Привет, ${name}!`,
  helloTemplate: name => `Здравствуй, ${name}!`,
  customMarkup: ({ firstName, lastName, fullName }) => (
    <article>
      Карточка пользователя
      <div>Имя: {firstName}</div>
      <div>Фамилия: {lastName}</div>
      <div>Полное имя: {fullName}</div>
    </article>
  )
};
