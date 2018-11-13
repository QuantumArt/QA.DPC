import React, { Component, ReactNode } from "react";
import qs from "qs";
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
import { ActionController } from "Services/ActionController";
import { EntityController } from "Services/EntityController";
import { RelationController } from "Services/RelationController";
import { InitializationController } from "Services/InitializationController";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { isFunction, isString, isObject } from "Utils/TypeChecks";
import {
  EditorSettings,
  EditorQueryParams,
  PublicationTrackerSettings
} from "Models/EditorSettingsModels";
import { FileController } from "Services/FileController";
import { SchemaLinker } from "Services/SchemaLinker";
import { SchemaCompiler } from "Services/SchemaCompiler";
import { PublicationContext } from "Services/PublicationContext";
import { PublicationTracker } from "Services/PublicationTracker";
import { OverlayPresenter } from "Services/OverlayPresenter";

/** Render-callback для отрисовки редактора корневой статьи после загрузки схемы и данных */
type RenderEditor = (entity: EntityObject, contentSchema: ContentSchema) => ReactNode;

interface ProductEditorProps {
  /** Обище настройки ProductEditor */
  editorSettings: EditorSettings;
  /** URL-параметры корневого CustomAction */
  queryParams?: EditorQueryParams | string;
  /** Настройки синхронизации статусов публикации */
  publicationTrackerSettings?: Partial<PublicationTrackerSettings>;
  /**
   * Настройка компонентов-редакторов для полей-связей по имени контента связи.
   * Переопределяются с помощью @see ArticleEditorProps.fieldEditors
   */
  relationEditors?: RelationsConfig;
  /** Render-callback для отрисовки редактора корневой статьи после загрузки данных */
  children?: RenderEditor | ReactNode;
}

/**
 * Корневой компонент редактора. Регистрирует необходимые сервисы, загружает схему и данные.
 */
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
  ActionController,
  EntityController,
  InitializationController,
  FileController,
  RelationController,
  OverlayPresenter,
  RelationsConfig,
  EditorSettings,
  EditorQueryParams,
  PublicationTrackerSettings
)
export class ProductEditor extends Component<ProductEditorProps> {
  @inject private _editorSettings: EditorSettings;
  @inject private _queryParams: EditorQueryParams;
  @inject private _relationsConfig: RelationsConfig;
  @inject private _publicationTrackerSettings: PublicationTrackerSettings;
  @inject private _initializationController: InitializationController;
  @inject private _overlayPresenter: OverlayPresenter;

  readonly state = {
    entity: null,
    contentSchema: null
  };

  constructor(props: ProductEditorProps, context?: any) {
    super(props, context);
    const { editorSettings, queryParams, relationEditors, publicationTrackerSettings } = this.props;
    Object.assign(this._editorSettings, editorSettings);
    Object.assign(this._relationsConfig, relationEditors);
    Object.assign(this._publicationTrackerSettings, publicationTrackerSettings);

    if (isObject(queryParams)) {
      Object.assign(this._queryParams, queryParams);
    } else {
      let queryString = isString(queryParams) ? queryParams : document.location.search;
      if (queryString.startsWith("?")) {
        queryString = queryString.slice(1);
      }
      Object.assign(this._queryParams, qs.parse(queryString));
    }
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
