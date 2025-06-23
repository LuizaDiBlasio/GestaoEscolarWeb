using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
using System;

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

    }
}
