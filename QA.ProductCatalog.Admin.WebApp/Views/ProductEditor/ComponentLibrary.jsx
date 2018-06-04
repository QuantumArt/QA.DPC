import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { Radio } from "@blueprintjs/core";
import { Grid, Row, Col } from "react-flexbox-grid";
import { observable } from "mobx";
import { observer } from "mobx-react";
import {
  InputText,
  InputNumber,
  InputSearch,
  CheckBox,
  TextArea,
  DatePicker,
  Select,
  RadioGroup
} from "Components/FormControls/FormControls";

const article = observable({
  StringField: "",
  PhoneField: "",
  NumericField: 0,
  SearchField: null,
  BooleanField: null,
  TextField: null,
  DateField: null,
  TimeField: null,
  DateTimeField: null,
  EnumField: "first",
  ArrayField: ["second"]
});

// prettier-ignore
const phoneMask = ["+", "7", " ", "(", /\d/, /\d/, /\d/, ")", " ", /\d/, /\d/, /\d/, "-", /\d/, /\d/, /\d/, /\d/];

const FormControlsBlock = observer(() => (
  <div>
    <h4>FormControls</h4>
    <hr />

    <Row className="pt-form-group">
      <Col md={3}>InputText [normal | disabled]</Col>
      <Col md={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
        />
      </Col>
      <Col md={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
          disabled
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>InputText [mask | readonly]</Col>
      <Col md={3}>
        <InputText
          name="PhoneField"
          model={article}
          placeholder="PhoneField"
          mask={phoneMask}
        />
      </Col>
      <Col md={3}>
        <InputText
          name="PhoneField"
          model={article}
          placeholder="PhoneField"
          readOnly
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>InputNumber [normal | disabled]</Col>
      <Col md={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
        />
      </Col>
      <Col md={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
          isInteger
          disabled
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>InputSearch [normal | disabled]</Col>
      <Col md={3}>
        <InputSearch
          name="SearchField"
          model={article}
          placeholder="SearchField"
        />
      </Col>
      <Col md={3}>
        <InputSearch
          name="SearchField"
          model={article}
          placeholder="SearchField"
          disabled
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>DatePicker [date | date disabled]</Col>
      <Col md={3}>
        <DatePicker
          name="DateField"
          model={article}
          type="date"
          placeholder="DateField"
        />
      </Col>
      <Col md={3}>
        <DatePicker
          name="DateField"
          model={article}
          type="date"
          placeholder="DateField"
          disabled
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>DatePicker [time | time disabled]</Col>
      <Col md={3}>
        <DatePicker
          name="TimeField"
          model={article}
          type="time"
          placeholder="TimeField"
        />
      </Col>
      <Col md={3}>
        <DatePicker
          name="TimeField"
          model={article}
          type="time"
          placeholder="TimeField"
          disabled
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>DatePicker [normal | disabled]</Col>
      <Col md={3}>
        <DatePicker
          name="DateTimeField"
          model={article}
          placeholder="DateTimeField"
        />
      </Col>
      <Col md={3}>
        <DatePicker
          name="DateTimeField"
          model={article}
          placeholder="DateTimeField"
          disabled
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>CheckBox [normal | disabled] </Col>
      <Col md={3}>
        <CheckBox name="BooleanField" model={article} inline />
      </Col>
      <Col md={3}>
        <CheckBox name="BooleanField" model={article} inline disabled />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>Select [normal | disabled]</Col>
      <Col md={3}>
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
      <Col md={3}>
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
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>Select [required | multiple]</Col>
      <Col md={3}>
        <Select
          name="EnumField"
          model={article}
          placeholder="EnumField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
          required
        />
      </Col>
      <Col md={3}>
        <Select
          name="ArrayField"
          model={article}
          placeholder="ArrayField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
          multiple
        />
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>RadioGroup [normal | disabled]</Col>
      <Col md={3}>
        <RadioGroup
          name="EnumField"
          model={article}
          placeholder="EnumField"
          inline
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" },
            { value: "third", label: "Третий", disabled: true }
          ]}
        />
      </Col>
      <Col md={6}>
        <RadioGroup
          name="EnumField"
          model={article}
          placeholder="EnumField"
          inline
          disabled
        >
          <Radio label="Первый" value="first" />
          <Radio label="Второй" value="second" />
        </RadioGroup>
      </Col>
    </Row>

    <Row className="pt-form-group">
      <Col md={3}>TextArea [normal]</Col>
      <Col md>
        <TextArea name="TextField" model={article} placeholder="TextField" />
      </Col>
    </Row>

    <hr />
    <code>
      <pre>{JSON.stringify(article, null, 2)}</pre>
    </code>
  </div>
));

ReactDOM.render(
  <Grid fluid>
    <FormControlsBlock />
  </Grid>,
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
//             <Col>My custom Col</Col>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Details: props => (
//           <div>
//             <Col>My custom Col</Col>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Type_Contents: {
//           InternetTariff: {
//             Description: props => (
//               <div>
//                 <Col>My custom Col</Col>
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
//             <Col>My custom Col</Col>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Details: props => (
//           <div>
//             <Col>My custom Col</Col>
//             <FieldEditor {...props} />
//           </div>
//         ),
//         Type_Contents: {
//           InternetTariff: {
//             Description: props => (
//               <div>
//                 <Col>My custom Col</Col>
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
//           <Col>My custom Col</Col>
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
//                   <Col>My custom Col</Col>
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
//               <Col>My custom Col</Col>
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
//                         <Col>My custom Col</Col>
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
//                         <Col>My custom Col</Col>
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
//                         <Col>My custom Col</Col>
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
