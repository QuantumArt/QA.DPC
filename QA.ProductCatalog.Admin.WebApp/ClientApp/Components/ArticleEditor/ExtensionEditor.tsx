import { observer } from "mobx-react";
import { ExtensionObject } from "Models/EditorDataModels";
import { AbstractEditor, ArticleEditorProps } from "./ArticleEditor";
import "./ArticleEditor.scss";

interface ExtensionEditorProps extends ArticleEditorProps {
  model: ExtensionObject;
}

/** Компонент для отображения и редактирования статьи-расширения */
@observer
export class ExtensionEditor extends AbstractEditor<ExtensionEditorProps> {}
