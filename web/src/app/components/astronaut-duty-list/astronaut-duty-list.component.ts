import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { PersonAstronaut, AstronautDuty } from '../../models/astronaut.model';

@Component({
  selector: 'app-astronaut-duty-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
  host: {
    class: 'astronaut-duty-list',
  },
  templateUrl: './astronaut-duty-list.component.html',
  styleUrl: './astronaut-duty-list.component.scss',
})
export class AstronautDutyListComponent {
  readonly person = input<PersonAstronaut | null>(null);
  readonly duties = input<AstronautDuty[]>([]);

  protected readonly activeDuty = computed(() => this.duties().find((duty) => !duty.dutyEndDate));

  protected readonly inactiveDuties = computed(() =>
    this.duties().filter((duty) => duty.dutyEndDate),
  );
}
