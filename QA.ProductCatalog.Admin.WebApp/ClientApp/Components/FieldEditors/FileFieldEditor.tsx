import React from "react";
import { Col } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { FileFieldSchema, FieldExactTypes } from "Models/EditorSchemaModels";
import { InputFile } from "Components/FormControls/FormControls";
import { required } from "Utils/Validators";
import { AbstractFieldEditor } from "./AbstractEditors";

// TODO: Интеграция с библиотекой QP

@observer
export class FileFieldEditor extends AbstractFieldEditor<FileFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: FileFieldSchema) {
    return (
      <Col xl={8} md={6}>
        <div className="pt-control-group pt-fill">
          <InputFile
            id={this.id}
            model={model}
            name={fieldSchema.FieldName}
            disabled={fieldSchema.IsReadOnly}
            accept={fieldSchema.FieldType === FieldExactTypes.Image ? "image/*" : ""}
            validate={fieldSchema.IsRequired && required}
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
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}
