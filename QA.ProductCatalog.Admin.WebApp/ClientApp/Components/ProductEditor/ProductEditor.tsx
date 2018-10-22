import React, { Component, ReactNode } from "react";
import { provider, inject } from "react-ioc";
import { Observer } from "mobx-react";
import { Grid } from "react-flexbox-grid";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationsConfig } from "Components/ArticleEditor/ArticleEditor";
import { DataContext } from "Services/DataContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import { DataMerger } from "Services/DataMerger";
import { DataValidator } from "Services/DataValidator";
import { EntityController } from "Services/EntityController";
import { RelationController } from "Services/RelationController";
import { InitializationController } from "Services/InitializationController";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { isFunction } from "Utils/TypeChecks";
import { EditorSettings } from "Models/EditorSettings";
import { FileController } from "Services/FileController";
import { SchemaLinker } from "Services/SchemaLinker";
import { SchemaCompiler } from "Services/SchemaCompiler";
import { PublicationContext } from "Services/PublicationContext";
import { PublicationTracker } from "Services/PublicationTracker";
import { OverlayPresenter } from "Services/OverlayPresenter";

type RenderEditor = (entity: EntityObject, contentSchema: ContentSchema) => ReactNode;

interface ProductEditorProps {
  settings: EditorSettings;
  relationEditors?: RelationsConfig;
  children?: RenderEditor | ReactNode;
}

@provider(
  DataContext,
  PublicationContext,
  PublicationTracker,
  DataNormalizer,
  DataSerializer,
  DataMerger,
  DataValidator,
  SchemaLinker,
  SchemaCompiler,
  EntityController,
  InitializationController,
  FileController,
  RelationController,
  OverlayPresenter,
  RelationsConfig,
  EditorSettings
)
export class ProductEditor extends Component<ProductEditorProps> {
  @inject private _editorSettings: EditorSettings;
  @inject private _relationsConfig: RelationsConfig;
  @inject private _initializationController: InitializationController;
  @inject private _overlayPresenter: OverlayPresenter;

  readonly state = {
    entity: null,
    contentSchema: null
  };

  constructor(props: ProductEditorProps, context?: any) {
    super(props, context);
    Object.assign(this._editorSettings, props.settings);
    Object.assign(this._relationsConfig, props.relationEditors);
  }

  async componentDidMount() {
    this.setState(await this._initializationController.initialize());
  }

  render() {
    const { children } = this.props;
    const { entity, contentSchema } = this.state;
    if (!entity || !contentSchema) {
      return null;
    }

    return (
      <>
        <Grid fluid>
          {isFunction(children) ? (
            children(entity, contentSchema)
          ) : (
            <EntityEditor model={entity} contentSchema={contentSchema} />
          )}
        </Grid>
        <Observer>{() => this._overlayPresenter.overlays.peek()}</Observer>
      </>
    );
  }
}
