.addCustomLinkButton({
	name: dstInputName,
	title: "Transliterate",
	suffix: "translit",
	class: "customLinkButton",
	url: "/Backend/Content/QP8/icons/16x16/insert_call.gif",
	onClick: function (evt) {
		var resultInput = evt.data.$input;
		var textToProcess = evt.data.$form.find("[name=" + srcInputName + "]").val();
		resultInput.val($ctx.getGlobal('Transliterator').transliterate(textToProcess));
	}
});
