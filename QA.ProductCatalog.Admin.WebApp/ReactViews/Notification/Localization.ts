import { Localization } from "Shared/Utils/Localization";

const strings = new Localization(window.notification);

export const l = strings.get;
