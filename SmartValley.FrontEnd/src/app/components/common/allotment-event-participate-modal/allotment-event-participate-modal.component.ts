import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material';
import {AllotmentEventParticipateDialogData} from './allotment-event-participate-dialog-data';
import {AllotmentEventsManagerContractClient} from '../../../services/contract-clients/allotment-events-manager-contract-client';
import BigNumber from 'bignumber.js';

@Component({
  selector: 'app-allotment-event-participate-modal',
  templateUrl: './allotment-event-participate-modal.component.html',
  styleUrls: ['./allotment-event-participate-modal.component.scss']
})
export class AllotmentEventParticipateModalComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: AllotmentEventParticipateDialogData,
              private allotmentEventsManagerContractClient: AllotmentEventsManagerContractClient,
              private participateModalComponent: MatDialogRef<AllotmentEventParticipateModalComponent>) { }

  public newBet: BigNumber;
  public frozenTime: Date;

  async ngOnInit() {
    const today = new Date();
    const freezingDuration = await this.allotmentEventsManagerContractClient.getFreezingDurationAsync();
    const nextMonth = today.setDate(today.getDate() + freezingDuration);
    this.frozenTime = new Date(nextMonth);
    this.data.myBet = new BigNumber(this.data.myBet) || new BigNumber(0);
  }

  public getComputedShare() {
      let myBid: BigNumber =  this.data.myBet;
      if (this.newBet) {
          myBid = myBid.plus(this.newBet);
      }

      return myBid.dividedBy(this.data.totalBet).mul(this.data.tokenBalance);
  }

  public submit() {
    this.participateModalComponent.close(this.newBet);
  }
}
