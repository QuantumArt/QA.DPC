import React from "react";
import { Col } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { FileFieldSchema, FieldExactTypes } from "Models/EditorSchemaModels";
import { InputFile } from "Components/FormControls/FormControls";
import { AbstractFieldEditor } from "./AbstractFieldEditor";

// TODO: Интеграция с библиотекой QP

@observer
export class FileFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: FileFieldSchema) {
    return (
      <Col xl md={6} className="field-editor__control field-editor__control--file">
        <div className="pt-control-group pt-fill">
          <InputFile
            id={this.id}
            model={model}
            name={fieldSchema.FieldName}
            disabled={fieldSchema.IsReadOnly}
            accept={fieldSchema.FieldType === FieldExactTypes.Image ? "image/*" : ""}
            className={cn({
              "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
            })}
          />
          {fieldSchema.FieldType === FieldExactTypes.Image && (
            <button className="pt-button pt-icon-eye-open" title="Просмотр" />
          )}
          <button className="pt-button pt-icon-cloud-download" title="Скачать" />
          <button className="pt-button pt-icon-folder-close" title="Библиотека" />
        </div>
      </Col>
    );
  }
}
