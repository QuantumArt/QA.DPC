import * as tslib_1 from "tslib";
import { observable, action } from "mobx";
var PublicationContext = /** @class */ (function() {
  function PublicationContext() {
    this._timestampsById = observable.map();
    this._maxPublicationTime = null;
  }
  Object.defineProperty(PublicationContext.prototype, "maxPublicationTime", {
    get: function() {
      return this._maxPublicationTime;
    },
    enumerable: true,
    configurable: true
  });
  PublicationContext.prototype.updateMaxPublicationTime = function(
    publicationTime
  ) {
    if (this._maxPublicationTime < publicationTime) {
      this._maxPublicationTime = publicationTime;
    }
  };
  PublicationContext.prototype.getLiveTime = function(product) {
    var serverId = product._ServerId;
    var timestamp = serverId && this._timestampsById.get(serverId);
    return timestamp && timestamp.Live;
  };
  PublicationContext.prototype.getStageTime = function(product) {
    var serverId = product._ServerId;
    var timestamp = serverId && this._timestampsById.get(serverId);
    return timestamp && timestamp.Stage;
  };
  PublicationContext.prototype.updateTimestamps = function(timestampsById) {
    var _this = this;
    Object.entries(timestampsById).forEach(function(_a) {
      var _b = tslib_1.__read(_a, 2),
        serverId = _b[0],
        timestamp = _b[1];
      var oldTimestamp = _this._timestampsById.get(Number(serverId));
      if (oldTimestamp) {
        if (oldTimestamp.Live < timestamp.Live) {
          oldTimestamp.Live = timestamp.Live;
        }
        if (oldTimestamp.Stage < timestamp.Stage) {
          oldTimestamp.Stage = timestamp.Stage;
        }
      } else {
        _this._timestampsById.set(Number(serverId), observable(timestamp));
      }
      _this.updateMaxPublicationTime(timestamp.Live);
      _this.updateMaxPublicationTime(timestamp.Stage);
    });
  };
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    PublicationContext.prototype,
    "updateTimestamps",
    null
  );
  return PublicationContext;
})();
export { PublicationContext };
//# sourceMappingURL=PublicationContext.js.map
