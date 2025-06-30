using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace GestaoEscolarWeb.Helpers
{
    public interface IConverterHelper
    {
        Course ToCourse(CourseViewModel model, bool isNew);

        CourseViewModel ToCourseViewModel(Course course);

        String ToShift(CreateEditSchoolClassViewModel model);

        int ToSelectedId(SchoolClass schoolClass);

        Student FromMyProfileToStudent(MyProfileViewModel model, bool isNew, Guid imageId);

        MyProfileViewModel ToMyProfileViewModel(Student student);

        CreateEditStudentViewModel ToCreateEditStudentViewModel(Student student);

        Student FromCreateEditToStudent(CreateEditStudentViewModel model, bool isNew, Guid image);

        EditEnrollmentViewModel ToEditEnrollmentViewModel(IEnumerable<SelectListItem> subjects, Enrollment enrollment, IEnumerable<SelectListItem> statusList);

        Enrollment ToEnrollment(EditEnrollmentViewModel model, Student student, bool isNew);

        CreateEditEvaluationViewModel ToCreateEditEvaluationViewModel(Evaluation evaluation, IEnumerable<SelectListItem> subjects);


        Evaluation ToEvaluation(CreateEditEvaluationViewModel model, Student student, bool IsNew);
    }
}
