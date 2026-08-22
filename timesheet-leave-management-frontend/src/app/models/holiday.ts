export interface Holiday {
  id: string;
  name: string;
  holidayDate: string;
  description: string | null;
  year: number;
  dayOfWeek: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateHolidayDto {
  name: string;
  holidayDate: string;
  description?: string;
}

export interface UpdateHolidayDto extends CreateHolidayDto {}

export interface CopyHolidayYearDto {
  sourceYear: number;
  targetYear: number;
  skipExistingDates: boolean;
}

export interface HolidayCheck {
  date: string;
  isHoliday: boolean;
  holidayId: string | null;
  holidayName: string | null;
}
