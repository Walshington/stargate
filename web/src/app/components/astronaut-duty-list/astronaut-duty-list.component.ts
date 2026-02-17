import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { PersonAstronaut, AstronautDuty } from '../../models/astronaut.model';

@Component({
  selector: 'app-astronaut-duty-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
  host: {
    'class': 'astronaut-duty-list',
  },
  templateUrl: './astronaut-duty-list.component.html',
  styleUrl: './astronaut-duty-list.component.scss',
})
export class AstronautDutyListComponent {
  person = input<PersonAstronaut | null>(null);
  duties = input<AstronautDuty[]>([]);

  activeDuty = computed(() => 
    this.duties().find(duty => !duty.dutyEndDate)
  );

  inactiveDuties = computed(() => 
    this.duties().filter(duty => duty.dutyEndDate)
  );
}
