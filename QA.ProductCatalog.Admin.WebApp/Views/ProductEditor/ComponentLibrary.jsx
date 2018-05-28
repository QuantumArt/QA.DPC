import "bootstrap/dist/css/bootstrap.css";
import React from "react";
import ReactDOM from "react-dom";
import { Col, FormGroup, Label } from "reactstrap";
import { observable } from "mobx";
import { observer } from "mobx-react";
import {
  InputText,
  InputNumber,
  InputSearch,
  CheckBox,
  TextArea,
  DatePicker,
  Select
} from "Components/FormControls/FormControls";

const article = observable({
  StringField: "",
  NumericField: 0,
  SearchField: null,
  BooleanField: null,
  TextField: null,
  DateField: null,
  EnumField: null
});

const FormControlsBlock = observer(() => (
  <div>
    <h4>FormControls</h4>
    <hr />

    <FormGroup row>
      <Label sm={3} size="sm">
        InputText
      </Label>
      <Col sm={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
        />
      </Col>
      <Col sm={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
          disabled
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        InputNumber
      </Label>
      <Col sm={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
        />
      </Col>
      <Col sm={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
          isInteger
          disabled
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        InputSearch
      </Label>
      <Col sm={3}>
        <InputSearch
          name="SearchField"
          model={article}
          placeholder="SearchField"
        />
      </Col>
      <Col sm={3}>
        <InputSearch
          name="SearchField"
          model={article}
          placeholder="SearchField"
          disabled
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        DatePicker
      </Label>
      <Col sm={3}>
        <DatePicker name="DateField" model={article} placeholder="DateField" />
      </Col>
      <Col sm={3}>
        <DatePicker
          name="DateField"
          model={article}
          placeholder="DateField"
          disabled
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        CheckBox
      </Label>
      <Col sm={3}>
        <CheckBox name="BooleanField" model={article} />
      </Col>
      <Col sm={3}>
        <CheckBox name="BooleanField" model={article} disabled />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        TextArea
      </Label>
      <Col sm={6}>
        <TextArea name="TextField" model={article} placeholder="TextField" />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        Select
      </Label>
      <Col sm={3}>
        <Select
          name="EnumField"
          model={article}
          placeholder="EnumField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
        />
      </Col>
      <Col sm={3}>
        <Select
          name="EnumField"
          model={article}
          placeholder="EnumField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
          disabled
        />
      </Col>
    </FormGroup>

    <hr />
    <code>
      <pre>{JSON.stringify(article, null, 2)}</pre>
    </code>
  </div>
));

ReactDOM.render(
  <div>
    <FormControlsBlock />
  </div>,
  document.getElementById("library")
);

// const productSchema = null;

// function ArticleEditor(props) {
//   return <div>{props.children}</div>;
// }

// ArticleEditor.Field = function(props) {
//   return <div>{props.children}</div>;
// };

// ArticleEditor.Fields = function(props) {
//   return <div>{props.children}</div>;
// };

// function field(_name, _render) {
//   return null;
// }

// function content(_name, _render) {
//   return null;
// }

// function FieldEditor(props) {
//   return <div>{props.children}</div>;
// }

// function ExtensionEditor(props) {
//   return <div>{props.children}</div>;
// }

// ExtensionEditor.Content = function(props) {
//   return <div>{props.children}</div>;
// };

// ExtensionEditor.Contents = function(props) {
//   return <div>{props.children}</div>;
// };

// const product = {
//   Id: 123,
//   Title: "Test",
//   Description: "Test test test...",
//   Regions: [{ Id: 40, Name: "Kaluga" }],
//   Devices: [{ Id: 5678, Region: { Id: 77, Name: "Moscow" } }]
// };

// const toDictionary = (keySelector, valueSelector = x => x) => (
//   result,
//   item
// ) => {
//   if (typeof result !== "object" || Array.isArray(result)) {
//     throw new TypeError("initialValue should be an Object");
//   }
//   const key = keySelector(item);
//   const value = valueSelector(item);

//   result[key] = value;
//   return result;
// };

// export default (
//   <div>
//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       include={{
//         Regions: true,
//         Devices: {
//           Region: true
//         }
//       }}
//       fields={{
//         Title: props => (
//           <div>
//             <label>My custom label</label>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Details: props => (
//           <div>
//             <label>My custom label</label>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Type_Contents: {
//           InternetTariff: {
//             Description: props => (
//               <div>
//                 <label>My custom label</label>
//                 <FieldEditor {...props} />
//               </div>
//             )
//           }
//         }
//       }}
//     />

//     <ArticleEditor model={product} schema={productSchema}>
//       {{
//         Title: props => (
//           <div>
//             <label>My custom label</label>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Details: props => (
//           <div>
//             <label>My custom label</label>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Type_Contents: {
//           InternetTariff: {
//             Description: props => (
//               <div>
//                 <label>My custom label</label>
//                 <FieldEditor {...props} />
//               </div>
//             )
//           }
//         }
//       }}
//     </ArticleEditor>

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       field-Type-InternetTariff-Description={props => (
//         <div>
//           <label>My custom label</label>
//           <FieldEditor {...props} />
//         </div>
//       )}
//     />

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       field-Type={props => (
//         <ExtensionEditor
//           {...props}
//           content-InternetTariff={props => (
//             <ArticleEditor
//               {...props}
//               field-Description={props => (
//                 <div>
//                   <label>My custom label</label>
//                   <FieldEditor {...props} />
//                 </div>
//               )}
//             />
//           )}
//         />
//       )}
//     />

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       {...Object.values(productSchema).reduce(
//         toDictionary(
//           s => `field-${s.FieldName}`,
//           () => props => (
//             <div>
//               <label>My custom label</label>
//               <FieldEditor {...props} />
//             </div>
//           )
//         )
//       )}
//     />

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       fields={[
//         field("Type", props => (
//           <ExtensionEditor
//             {...props}
//             contents={[
//               content("InternetTariff", props => (
//                 <ArticleEditor
//                   {...props}
//                   fields={[
//                     field("Description", props => (
//                       <div>
//                         <label>My custom label</label>
//                         <FieldEditor {...props} />
//                       </div>
//                     ))
//                   ]}
//                 />
//               ))
//             ]}
//           />
//         ))
//       ]}
//     />

//     <ArticleEditor model={product} schema={productSchema}>
//       <ArticleEditor.Fields
//         Type={props => (
//           <ExtensionEditor {...props}>
//             <ExtensionEditor.Contents
//               InternetTariff={props => (
//                 <ArticleEditor {...props}>
//                   <ArticleEditor.Fields
//                     Description={props => (
//                       <div>
//                         <label>My custom label</label>
//                         <FieldEditor {...props} />
//                       </div>
//                     )}
//                   />
//                 </ArticleEditor>
//               )}
//             />
//           </ExtensionEditor>
//         )}
//       />
//     </ArticleEditor>

//     <ArticleEditor model={product} schema={productSchema}>
//       <ArticleEditor.Field name="Type">
//         {props => (
//           <ExtensionEditor {...props}>
//             <ExtensionEditor.Content name="InternetTariff">
//               {props => (
//                 <ArticleEditor {...props}>
//                   <ArticleEditor.Field name="Description">
//                     {props => (
//                       <div>
//                         <label>My custom label</label>
//                         <FieldEditor {...props} />
//                       </div>
//                     )}
//                   </ArticleEditor.Field>
//                 </ArticleEditor>
//               )}
//             </ExtensionEditor.Content>
//           </ExtensionEditor>
//         )}
//       </ArticleEditor.Field>
//     </ArticleEditor>

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       field-Title={(model, fieldSchema) => (
//         <FieldEditor model={model} schema={fieldSchema} />
//       )}
//       field-Description={(model, fieldSchema) => (
//         <FieldEditor model={model} schema={fieldSchema} />
//       )}
//     />

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       fields={{
//         Title: (model, fieldSchema) => (
//           <FieldEditor model={model} schema={fieldSchema} />
//         ),
//         Description: (model, fieldSchema) => (
//           <FieldEditor model={model} schema={fieldSchema} />
//         )
//       }}
//     />

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       fields={[
//         field("Title", (model, fieldSchema) => (
//           <FieldEditor model={model} schema={fieldSchema} />
//         )),
//         field("Description", (model, fieldSchema) => (
//           <FieldEditor model={model} schema={fieldSchema} />
//         ))
//       ]}
//     />

//     <ArticleEditor
//       model={product}
//       schema={productSchema}
//       fields={Object.values(productSchema.Fields).map(({ FieldName }) =>
//         field(FieldName, (model, fieldSchema) => (
//           <>
//             <div>My custom text</div>
//             <FieldEditor model={model} schema={fieldSchema} />
//           </>
//         ))
//       )}
//     />

//     <ArticleEditor model={product} schema={productSchema}>
//       <ArticleEditor.Fields
//         Title={(model, fieldSchema) => (
//           <FieldEditor model={model} schema={fieldSchema} />
//         )}
//         Description={(model, fieldSchema) => (
//           <FieldEditor model={model} schema={fieldSchema} />
//         )}
//       />
//     </ArticleEditor>

//     <ArticleEditor model={product} schema={productSchema}>
//       {Object.values(productSchema.Fields).map(fieldSchema => (
//         <ArticleEditor.Field
//           name={fieldSchema.FieldName}
//           render={(model, fieldSchema) => (
//             <div>
//               {fieldSchema.FeildDescription}
//               <FieldEditor model={model} schema={fieldSchema} />
//             </div>
//           )}
//         />
//       ))}
//     </ArticleEditor>
//   </div>
// );
