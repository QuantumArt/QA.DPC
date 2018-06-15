import React from "react";
import { Row } from "react-flexbox-grid";
import { Button } from "@blueprintjs/core";
import { inject } from "react-ioc";
import { observer } from "mobx-react";
import { ArticleObject } from "Models/EditorDataModels";
import { RelationSelection, validateRelationSelection } from "Models/RelationSelection";
import { DataSerializer } from "Services/DataSerializer";
import { ObjectEditor, ObjectEditorProps } from "./ObjectEditor";
export { IGNORE, FieldsConfig } from "./ObjectEditor";
import "./ArticleEditor.scss";

interface ArticleEditorProps {
  model: ArticleObject;
  save?: boolean;
  saveRelations?: RelationSelection;
}

@observer
export class ArticleEditor extends ObjectEditor<ArticleEditorProps> {
  @inject private _dataSerializer: DataSerializer;

  constructor(props: ObjectEditorProps & ArticleEditorProps, context?: any) {
    super(props, context);
    const { contentSchema, saveRelations } = this.props;
    if (saveRelations) {
      validateRelationSelection(contentSchema, saveRelations);
    }
  }

  render() {
    const { model, contentSchema, save } = this.props;
    const serverId = this._dataSerializer.getServerId(model);
    return (
      <>
        {save && (
          <div className="article-editor__header">
            <div className="article-editor__title" title={contentSchema.ContentDescription}>
              {contentSchema.ContentTitle || contentSchema.ContentName}
              {serverId > 0 && `: ${serverId}`}
            </div>
            {save && <Button icon="floppy-disk">Сохранить</Button>}
          </div>
        )}
        <Row>{super.render()}</Row>
      </>
    );
  }
}
