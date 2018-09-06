import React from "react";
import { Col } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject } from "Models/EditorDataModels";
import { FileFieldSchema, FieldExactTypes } from "Models/EditorSchemaModels";
import { InputFile } from "Components/FormControls/FormControls";
import { AbstractFieldEditor } from "./AbstractFieldEditor";
import { consumer, inject } from "react-ioc";
import { FileController } from "Services/FileController";
import { action } from "mobx";

// TODO: Интеграция с библиотекой QP

@consumer
@observer
export class FileFieldEditor extends AbstractFieldEditor {
  @inject private _fileController: FileController;

  @action
  previewImage = () => {
    const { model, fieldSchema } = this.props;
    const fileName = model[fieldSchema.FieldName];
    if (fileName) {
      this._fileController.previewImage(model, fieldSchema as FileFieldSchema);
    }
  };

  @action
  downloadFile = () => {
    const { model, fieldSchema } = this.props;
    const fileName = model[fieldSchema.FieldName];
    if (fileName) {
      this._fileController.downloadFile(model, fieldSchema as FileFieldSchema);
    }
  };

  renderField(model: ArticleObject, fieldSchema: FileFieldSchema) {
    return (
      <Col xl md={6} className="file-field-editor">
        <div className="pt-control-group pt-fill">
          <InputFile
            id={this.id}
            model={model}
            name={fieldSchema.FieldName}
            disabled={fieldSchema.IsReadOnly}
            accept={fieldSchema.FieldType === FieldExactTypes.Image ? "image/*" : ""}
            className={cn({
              "pt-intent-primary": model.isEdited(fieldSchema.FieldName),
              "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
            })}
          />
          {fieldSchema.FieldType === FieldExactTypes.Image && (
            <button
              className="pt-button pt-icon-media"
              title="Просмотр"
              onClick={this.previewImage}
            />
          )}
          <button
            className="pt-button pt-icon-cloud-download"
            title="Скачать"
            onClick={this.downloadFile}
          />
          <button className="pt-button pt-icon-folder-close" title="Библиотека" />
        </div>
      </Col>
    );
  }
}
