export const setBrowserNotifications = (sendNotifyCb: () => void) => {
  // Let's check if the browser supports notifications
  if (!("Notification" in window)) {
    if (
      window.navigator.userAgent.indexOf("MSIE ") > 0 ||
      !!navigator.userAgent.match(/Trident.*rv\:11\./)
    ) {
      const sessionStorageIeAlertedKey = "IeAlerted";
      if (!window.sessionStorage.getItem(sessionStorageIeAlertedKey)) {
        window.setTimeout(function() {
          alert(
            "Браузер Internet Explorer не поддерживает html5 уведомления. Рекомендуется использовать любой из браузеров: Chrome, Firefox, Safari или Opera."
          );
        }, 10);
        window.sessionStorage.setItem(sessionStorageIeAlertedKey, "1");
      }
    }
    return;
  }

  if (Notification.permission === "denied") {
    return;
  }

  if (Notification.permission !== "granted") {
    if (!window.notificationPermissionRequested) {
      window.notificationPermissionRequested = true;

      Notification.requestPermission().then(result => {
        if (result === "granted") setBrowserNotifications(sendNotifyCb);
      });
    }
    return;
  }

  sendNotifyCb();
};

export const checkPermissions = async () => {
  const notificationPerm = await navigator.permissions.query({ name: "notifications" });
  console.log(notificationPerm);
  if (notificationPerm.state !== "granted") {
    window.open("/AppInfo", "AppInfo", "resizable,location,width=500,height=150");
  }
}
