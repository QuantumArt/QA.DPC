import React, { Component, ReactNode } from "react";
import { provider, inject } from "react-ioc";
import { Grid } from "react-flexbox-grid";
import { EntityEditor, RelationsConfig } from "Components/ArticleEditor/EntityEditor";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import { DataMerger } from "Services/DataMerger";
import { DataValidator } from "Services/DataValidator";
import { ArticleController } from "Services/ArticleController";
import { RelationController } from "Services/RelationController";
import { EditorController } from "Services/EditorController";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { isFunction } from "Utils/TypeChecks";
import { EditorSettings } from "Models/EditorSettings";

type RenderEditor = (article: EntityObject, contentSchema: ContentSchema) => ReactNode;

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
  DataMerger,
  DataValidator,
  ArticleController,
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

    const contentSchema = this._schemaContext.rootSchema;
    return (
      <Grid fluid>
        {isFunction(children) ? (
          children(article, contentSchema)
        ) : (
          <EntityEditor model={article} contentSchema={contentSchema} />
        )}
      </Grid>
    );
  }
}
