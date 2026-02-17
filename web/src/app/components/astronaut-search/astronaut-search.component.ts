import { Component, ChangeDetectionStrategy, output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-astronaut-search',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule],
  host: {
    class: 'astronaut-search',
  },
  templateUrl: './astronaut-search.component.html',
  styleUrl: './astronaut-search.component.scss',
})
export class AstronautSearchComponent {
  searchName = signal('');
  astronautSearched = output<string>();

  protected readonly isSearchDisabled = computed(() => {
    const name = this.searchName().trim();
    return name.length === 0;
  });

  protected onSearch(): void {
    const name = this.searchName().trim();
    if (name) {
      this.astronautSearched.emit(name);
    }
  }
}
