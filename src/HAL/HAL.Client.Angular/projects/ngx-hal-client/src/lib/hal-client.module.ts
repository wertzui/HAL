import { NgModule } from '@angular/core';
import { HalClient } from './hal-client';



@NgModule({
  declarations: [
  ],
  imports: [
  ],
  exports: [
  ],
  providers: [
    HalClient
  ]
})
export class HalClientModule { 
  private static _defaultToJson = Date.prototype.toJSON;

  constructor() {
    HalClientModule.EnsureJsonPreservesTimeZoneInformation();
  }

  public static EnsureJsonPreservesTimeZoneInformation() {
    Date.prototype.toJSON = function () {
      let offSetMins = this.getTimezoneOffset();
      const msInMin = 6e4;
      const date = new Date(this.getTime() - offSetMins * msInMin);
      const stringDate = date.toISOString().slice(0, -1);
      const isNegative = offSetMins < 0
      if (isNegative) 
        offSetMins = offSetMins * -1
      const hours = offSetMins / 60
      const hoursStr = (hours - hours % 1).toString()
      const minsStr = (offSetMins % 60).toString()
      const padTime = (n: string) => n.length == 1 ? '0' + n : n
      const stringOffset = `${isNegative || offSetMins == 0 ? '+' : '-'}${padTime(hoursStr)}:${padTime(minsStr)}`
      return stringDate + stringOffset;
    }
  }

  public static RestoreDefaultToJson() {
    Date.prototype.toJSON = HalClientModule._defaultToJson;
  }
}
