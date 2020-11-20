import React, { Component } from "react";
import { Checkbox, Button, Intent, Icon } from "@blueprintjs/core";
import { FieldEditorProps } from "ProductEditor/Components/ArticleEditor/ArticleEditor";
import { MultiRelationFieldTable } from "ProductEditor/Components/FieldEditors/FieldEditors";
import { Advantage } from "../TypeScriptSchema";
import { RelationFieldSchema, FileFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { inject } from "react-ioc";
import { FileController } from "ProductEditor/Services/FileController";

export class AdvantagesTable extends Component<FieldEditorProps> {
  @inject private _fileController: FileController;

  private previewImage(advantage: Advantage) {
    const relationFieldSchema = this.props.fieldSchema as RelationFieldSchema;
    const fileFieldSchema = relationFieldSchema.RelatedContent.Fields.ImageSvg as FileFieldSchema;
    this._fileController.previewImage(advantage, fileFieldSchema);
  }

  render() {
    return (
      <MultiRelationFieldTable
        {...this.props}
        displayFields={[
          this.renderTitle,
          this.renderIsGift,
          this.renderImageSvg,
          this.renderPreviewButton
        ]}
      />
    );
  }

  private renderTitle = (advantage: Advantage) => advantage.Title;

  private renderIsGift = (advantage: Advantage) => (
    <div className="advantages-table__is-gift">
      IsGift: <Checkbox checked={advantage.IsGift} disabled />
    </div>
  );

  private renderImageSvg = (advantage: Advantage) => advantage.ImageSvg;

  private renderPreviewButton = (advantage: Advantage) =>
    advantage.ImageSvg && (
      <Button
        small
        minimal
        icon={<Icon icon="media" title="Просмотр" />}
        intent={Intent.PRIMARY}
        onClick={() => this.previewImage(advantage)}
      />
    );
}
