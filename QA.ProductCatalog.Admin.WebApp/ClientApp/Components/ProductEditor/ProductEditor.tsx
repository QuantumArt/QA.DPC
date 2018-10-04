import React, { Component, ReactNode } from "react";
import { provider, inject } from "react-ioc";
import { Grid } from "react-flexbox-grid";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationsConfig } from "Components/ArticleEditor/ArticleEditor";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import { DataMerger } from "Services/DataMerger";
import { DataValidator } from "Services/DataValidator";
import { EntityController } from "Services/EntityController";
import { RelationController } from "Services/RelationController";
import { ProductController } from "Services/ProductController";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { isFunction } from "Utils/TypeChecks";
import { EditorSettings } from "Models/EditorSettings";
import { CloneController } from "Services/CloneController";
import { FileController } from "Services/FileController";
import { DataSchemaLinker } from "Services/DataSchemaLinker";

type RenderEditor = (entity: EntityObject, contentSchema: ContentSchema) => ReactNode;

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
  DataSchemaLinker,
  EntityController,
  CloneController,
  ProductController,
  FileController,
  RelationController,
  RelationsConfig,
  EditorSettings
)
export class ProductEditor extends Component<ProductEditorProps> {
  @inject private _editorSettings: EditorSettings;
  @inject private _relationsConfig: RelationsConfig;
  @inject private _productController: ProductController;
  @inject private _schemaContext: SchemaContext;
  readonly state = {
    entity: null
  };

  constructor(props: ProductEditorProps, context?: any) {
    super(props, context);
    Object.assign(this._editorSettings, props.settings);
    if (props.relationEditors) {
      Object.assign(this._relationsConfig, props.relationEditors);
    }
  }

  async componentDidMount() {
    const entity = await this._productController.initialize();
    this.setState({ entity });
  }

  render() {
    const { children } = this.props;
    const { entity } = this.state;
    if (!entity) {
      return null;
    }

    const contentSchema = this._schemaContext.rootSchema;
    return (
      <Grid fluid>
        {isFunction(children) ? (
          children(entity, contentSchema)
        ) : (
          <EntityEditor model={entity} contentSchema={contentSchema} />
        )}
      </Grid>
    );
  }
}
