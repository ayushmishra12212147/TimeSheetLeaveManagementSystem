import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'istDate',
})
export class IstDatePipe implements PipeTransform {
  transform(value: unknown, ...args: unknown[]): unknown {
    return null;
  }
}
