import React, { Component } from "react";
import { observer } from "mobx-react";
import { Input, Checkbox } from "../FormControls/FormControls";

type Props = {
  article: any;
  contentSchema: any;
};

@observer
export class ArticleEditor extends Component<Props> {
  render() {
    const { contentSchema, article } = this.props;
    const fields = Object.values(contentSchema.Fields);

    return <div>{fields.map(f => this.renderField(f, article))}</div>;
  }

  renderField(fieldSchema, article) {
    switch (fieldSchema.FieldType) {
      case "String":
        return <Input model={article} name={fieldSchema.FieldName} />;
      case "Boolean":
        return <Checkbox model={article} name={fieldSchema.FieldName} />;
      default:
        return null;
    }
  }
}
