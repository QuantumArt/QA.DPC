import React from "react";

export default {
  "Test...": `Test...`,
  "Hello, ${name}!": name => `Hello, ${name}!`,
  helloTemplate: name => `Hello, ${name}!`,
  customComponent: ({ firstName, lastName, fullName }) => (
    <article key={1}>
      User Card
      <div key={2}>First Name: {firstName}</div>
      <div key={3}>Last Name: {lastName}</div>
      <div key={4}>Full Name: {fullName}</div>
    </article>
  )
};
