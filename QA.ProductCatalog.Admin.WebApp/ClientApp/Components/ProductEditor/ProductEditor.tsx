import React, { Component, ReactNode } from "react";
import { provider, inject } from "react-ioc";
import { Grid } from "react-flexbox-grid";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
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
  children?: RenderEditor | ReactNode;
}

@provider(DataContext, SchemaContext, DataNormalizer, DataSerializer, EditorController)
export class ProductEditor extends Component<ProductEditorProps> {
  @inject private _editorController: EditorController;
  @inject private _schemaContext: SchemaContext;

  readonly state = {
    article: null
  };

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
