import React, { Component, ReactNode } from "react";
import { provider, inject } from "react-ioc";
import { Grid } from "react-flexbox-grid";
import { onPatch } from "mobx-state-tree";
import { ArticleEditor, RelationsConfig } from "Components/ArticleEditor/ArticleEditor";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import { EditorController } from "Services/EditorController";
import { ArticleObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { isFunction } from "Utils/TypeChecks";

type RenderEditor = (article: ArticleObject, contentSchema: ContentSchema) => ReactNode;

interface ProductEditorProps {
  relationEditors?: RelationsConfig;
  children?: RenderEditor | ReactNode;
}

@provider(
  DataContext,
  SchemaContext,
  DataNormalizer,
  DataSerializer,
  EditorController,
  RelationsConfig
)
export class ProductEditor extends Component<ProductEditorProps> {
  @inject private _relationsConfig: RelationsConfig;
  @inject private _editorController: EditorController;
  @inject private _schemaContext: SchemaContext;
  readonly state = {
    article: null
  };

  constructor(props: ProductEditorProps, context?: any) {
    super(props, context);
    if (props.relationEditors) {
      Object.assign(this._relationsConfig, props.relationEditors);
    }
  }

  async componentDidMount() {
    const article = await this._editorController.initialize();
    if (DEBUG) {
      onPatch(article, patch => console.log(patch));
    }
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
