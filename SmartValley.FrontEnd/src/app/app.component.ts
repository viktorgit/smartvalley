import {Component, OnInit} from '@angular/core';
import {Options} from 'angular2-notifications';
import {QuestionService} from './services/question-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  notifyOptions: Options = {
    position: ['top', 'left'],
    timeOut: 3000,
    animate: 'fromLeft',
    showProgressBar: false
  };
  constructor(private questionService: QuestionService) {
  }
  async ngOnInit() {
    await this.questionService.initializeQestionsCollectionAsync();
  }
}
