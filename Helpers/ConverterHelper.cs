using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestaoEscolarWeb.Helpers
{
    public class ConverterHelper : IConverterHelper
    {

        public Course ToCourse(CourseViewModel model, bool isNew)
        {
            return new Course
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                StartDate = model.StartDate.Value,
                EndDate = model.EndDate.Value
            };
        }

        public CourseViewModel ToCourseViewModel(Course course)
        {
            return new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CourseSubjects = course.CourseSubjects,
                SelectedSubjectIds = course.CourseSubjects? //evitar erro caso CourseSubjects seja null
                                   .Select(s => s.Id) //selecionar por id
                                   .ToList() ?? new List<int>() // se a lista criada pela busca for nula, criar uma nova lista

            };

        }

        public Data.Entities.Student FromMyProfileToStudent(MyProfileViewModel model, bool isNew, Guid imageId)
        {
            return new Data.Entities.Student
            {
                Id = isNew ? 0 : model.Id,
                FullName = model.FullName,
                Email = model.Email,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate.Value,
                ProfileImageId = imageId
            };

        }

        public MyProfileViewModel ToMyProfileViewModel(Data.Entities.Student student)
        {
            return new MyProfileViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                BirthDate = student.BirthDate,
                ProfileImageId = student.ProfileImageId
            };
        }

        public Data.Entities.Student FromCreateEditToStudent(CreateEditStudentViewModel model, bool isNew, Guid imageId)
        {
            return new Data.Entities.Student
            {
                Id = isNew ? 0 : model.Id,
                FullName = model.FullName,
                Email = model.Email,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate.Value,
                ProfileImageId = imageId,
                SchoolClassId = model.SelectedSchoolClassId
            };
        }

        public CreateEditStudentViewModel ToCreateEditStudentViewModel(Data.Entities.Student student)
        {

            return new CreateEditStudentViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                BirthDate = student.BirthDate,
                ProfileImageId = student.ProfileImageId,
                SelectedSchoolClassId = student.SchoolClassId
            };
        }

        public string ToShift(CreateEditSchoolClassViewModel model)
        {
            string shift = string.Empty;

            switch (model.SelectedShiftId)
            {
                case 1:
                    shift = "Morning";
                    break;
                case 2:
                    shift = "Afternoon";
                    break;
                case 3:
                    shift = "Night";
                    break;
                default:
                    shift = null;
                    break;
            }

            return shift;
        }

        public int ToSelectedId(SchoolClass schoolClass)
        {
            int selectedId = 0;

            switch (schoolClass.Shift)
            {
                case "Morning":
                    selectedId = 1;
                    break;
                case "Afternoon":
                    selectedId = 2;
                    break;
                case "Night":
                    selectedId = 3;
                    break;
            }

            return selectedId;
        }

        public EditEnrollmentViewModel ToEditEnrollmentViewModel(IEnumerable<SelectListItem> subjects, Enrollment enrollment, IEnumerable<SelectListItem> statusList)
        {
            return new EditEnrollmentViewModel()
            {
                Id = enrollment.Id,
                StudentFullName = enrollment.Student.FullName,
                SelectedSubjectId = enrollment.SubjectId,
                StudentStatus = enrollment.StudentStatus,
                EnrollmentDate = enrollment.EnrollmentDate,
                AbscenceRecord = enrollment.AbscenceRecord,
                Subjects = subjects,
                StudentStatusList = statusList
            };
        }

        public Enrollment ToEnrollment(EditEnrollmentViewModel model, Data.Entities.Student student, bool isNew)
        {
            return new Enrollment()
            {
                Id = isNew? 0 : model.Id,
                StudentId = student.Id,
                StudentStatus = model.StudentStatus,
                SubjectId = model.SelectedSubjectId,
                EnrollmentDate = model.EnrollmentDate,
                AbscenceRecord = model.AbscenceRecord
            };
        }

        public CreateEditEvaluationViewModel ToCreateEditEvaluationViewModel(Evaluation evaluation, IEnumerable<SelectListItem> subjects)
        {
            var model = new CreateEditEvaluationViewModel()
            {
                Id = evaluation.Id,
                StudentFullName = evaluation.Student.FullName,
                ExamDate = evaluation.ExamDate,
                SelectedSubjectId = evaluation.SubjectId,
                Score = evaluation.Score,
                Subjects = subjects
            };

            return model;
        }

        public Evaluation ToEvaluation(CreateEditEvaluationViewModel model, Data.Entities.Student student,  bool IsNew)
        {
            var evaluation = new Evaluation()
            {   
                Id = IsNew ? 0 : model.Id,
                StudentId = student.Id,
                ExamDate = model.ExamDate.Value,
                SubjectId = model.SelectedSubjectId,
                Score = model.Score.Value,
            };

            return evaluation;  
        }

    }
}
