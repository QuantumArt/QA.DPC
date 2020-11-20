import React from "react";
import { ArticleErrors } from "ProductEditor/Services/DataValidator";
import "./ValidationSummay.scss";

interface ValidationSummayProps {
  errors: ArticleErrors[];
}

export const ValidationSummay = ({ errors }: ValidationSummayProps) => {
  return (
    <>
      {errors.map((articleError, i) => (
        <section key={i} className="validation-summary__block">
          <header>
            <b>{articleError.ContentName}:</b> {articleError.ServerId}
          </header>
          {articleError.ArticleErrors.length > 0 && (
            <span className="validation-summary__message">
              {articleError.ArticleErrors.join(", ")}
            </span>
          )}
          {articleError.FieldErrors.length > 0 && (
            <main>
              {articleError.FieldErrors.map((fieldError, j) => (
                <div key={j}>
                  <u>{fieldError.Name}:</u>{" "}
                  <span className="validation-summary__message">
                    {fieldError.Messages.join(", ")}
                  </span>
                </div>
              ))}
            </main>
          )}
        </section>
      ))}
    </>
  );
};
