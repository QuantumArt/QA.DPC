import React from "react";
import { Col } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject } from "Models/EditorDataModels";
import { FileFieldSchema, FieldExactTypes } from "Models/EditorSchemaModels";
import { InputFile } from "Components/FormControls/FormControls";
import { AbstractFieldEditor, FieldEditorProps } from "./AbstractFieldEditor";
import { inject } from "react-ioc";
import { FileController } from "Services/FileController";
import { action } from "mobx";

interface FileFieldEditorProps extends FieldEditorProps {
  /** Опциональный подкаталог, который добавляется справа к `fieldSchema.SubFolder` */
  customSubFolder?: string;
}

@observer
export class FileFieldEditor extends AbstractFieldEditor<FileFieldEditorProps> {
  @inject private _fileController: FileController;

  @action
  private previewImage = () => {
    const { model, fieldSchema } = this.props;
    const fileName = model[fieldSchema.FieldName];
    if (fileName) {
      this._fileController.previewImage(model, fieldSchema as FileFieldSchema);
    }
  };

  @action
  private downloadFile = () => {
    const { model, fieldSchema } = this.props;
    const fileName = model[fieldSchema.FieldName];
    if (fileName) {
      this._fileController.downloadFile(model, fieldSchema as FileFieldSchema);
    }
  };

  @action
  private selectFile = () => {
    const { model, fieldSchema, readonly, customSubFolder } = this.props;
    if (!readonly) {
      this._fileController.selectFile(model, fieldSchema as FileFieldSchema, customSubFolder);
    }
  };

  renderField(model: ArticleObject, fieldSchema: FileFieldSchema) {
    return (
      <Col xl md={6} className="file-field-editor">
        <div className="bp3-control-group bp3-fill">
          <InputFile
            id={this._id}
            model={model}
            name={fieldSchema.FieldName}
            placeholder={this.getPlaceholder()}
            disabled={this._readonly}
            readOnly={true}
            accept={fieldSchema.FieldType === FieldExactTypes.Image ? "image/*" : ""}
            className={cn({
              "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
              "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
            })}
          />
          {fieldSchema.FieldType === FieldExactTypes.Image && (
            <button
              className="bp3-button bp3-icon-media"
              title="Просмотр"
              onClick={this.previewImage}
            />
          )}
          <button
            className="bp3-button bp3-icon-cloud-download"
            title="Скачать"
            onClick={this.downloadFile}
          />
          <button
            className="bp3-button bp3-icon-folder-close"
            title="Библиотека"
            onClick={this.selectFile}
          />
        </div>
      </Col>
    );
  }

  private getPlaceholder() {
    const { customSubFolder } = this.props;
    return customSubFolder
      ? customSubFolder.endsWith("/")
        ? customSubFolder
        : customSubFolder + "/"
      : "";
  }
}
