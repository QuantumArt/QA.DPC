import React, { Component, ReactNode } from "react";
import qs from "qs";
import { provider, inject } from "react-ioc";
import { Observer } from "mobx-react";
import { Grid } from "react-flexbox-grid";
import { LocaleContext } from "ProductEditor/Packages/react-lazy-i18n";
import { EntityEditor } from "ProductEditor/Components/ArticleEditor/EntityEditor";
import { RelationsConfig } from "ProductEditor/Components/ArticleEditor/ArticleEditor";
import { DataContext } from "ProductEditor/Services/DataContext";
import { DataNormalizer } from "ProductEditor/Services/DataNormalizer";
import { DataSerializer } from "ProductEditor/Services/DataSerializer";
import { DataMerger } from "ProductEditor/Services/DataMerger";
import { DataValidator } from "ProductEditor/Services/DataValidator";
import { ActionController } from "ProductEditor/Services/ActionController";
import { EntityController } from "ProductEditor/Services/EntityController";
import { RelationController } from "ProductEditor/Services/RelationController";
import { InitializationController } from "ProductEditor/Services/InitializationController";
import { EntityObject } from "ProductEditor/Models/EditorDataModels";
import { ContentSchema } from "ProductEditor/Models/EditorSchemaModels";
import { isFunction, isString, isObject } from "ProductEditor/Utils/TypeChecks";
import {
  EditorSettings,
  EditorQueryParams,
  PublicationTrackerSettings
} from "ProductEditor/Models/EditorSettingsModels";
import { FileController } from "ProductEditor/Services/FileController";
import { SchemaLinker } from "ProductEditor/Services/SchemaLinker";
import { SchemaCompiler } from "ProductEditor/Services/SchemaCompiler";
import { PublicationContext } from "ProductEditor/Services/PublicationContext";
import { PublicationTracker } from "ProductEditor/Services/PublicationTracker";
import { OverlayPresenter } from "ProductEditor/Services/OverlayPresenter";

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
      <LocaleContext.Provider value={this._editorSettings.UserLocale}>
        <Grid fluid>
          {isFunction(children) ? (
            children(entity, contentSchema)
          ) : (
            <EntityEditor model={entity} contentSchema={contentSchema} />
          )}
        </Grid>
        <Observer>{() => this._overlayPresenter.overlays.peek()}</Observer>
      </LocaleContext.Provider>
    );
  }
}
