import React, { Component, ReactNode } from "react";
import { provider, inject } from "react-ioc";
import { Grid } from "react-flexbox-grid";
import { ArticleEditor, RelationsConfig } from "Components/ArticleEditor/ArticleEditor";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import { RelationController } from "Services/RelationController";
import { EditorController } from "Services/EditorController";
import { ArticleObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { isFunction } from "Utils/TypeChecks";
import { EditorSettings } from "Models/EditorSettings";

type RenderEditor = (article: ArticleObject, contentSchema: ContentSchema) => ReactNode;

interface ProductEditorProps {
  settings: EditorSettings;
  relationEditors?: RelationsConfig;
  children?: RenderEditor | ReactNode;
}

@provider(
  DataContext,
  SchemaContext,
  DataNormalizer,
  DataSerializer,
  EditorController,
  RelationController,
  RelationsConfig,
  EditorSettings
)
export class ProductEditor extends Component<ProductEditorProps> {
  @inject private _editorSettings: EditorSettings;
  @inject private _relationsConfig: RelationsConfig;
  @inject private _editorController: EditorController;
  @inject private _schemaContext: SchemaContext;
  readonly state = {
    article: null
  };

  constructor(props: ProductEditorProps, context?: any) {
    super(props, context);
    Object.assign(this._editorSettings, props.settings);
    if (props.relationEditors) {
      Object.assign(this._relationsConfig, props.relationEditors);
    }
  }

  async componentDidMount() {
    const article = await this._editorController.initialize();
    this.setState({ article });
  }

  render() {
    const { children } = this.props;
    const { article } = this.state;
    if (!article) {
      return null;
    }

    const contentSchema = this._schemaContext.contentSchema;
    return (
      <Grid fluid>
        {isFunction(children) ? (
          children(article, contentSchema)
        ) : (
          <ArticleEditor model={article} contentSchema={contentSchema} />
        )}
      </Grid>
    );
  }
}
