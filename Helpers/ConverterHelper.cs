using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

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
                StartDate = model.StartDate,
                EndDate = model.EndDate
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
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                ProfileImageId = imageId 
            };
            
        }

        public MyProfileViewModel ToMyProfileViewModel(Data.Entities.Student student)
        {
            return new MyProfileViewModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                BirthDate = student.BirthDate,
                ProfileImageId = student.ProfileImageId
            };
        }

        public Data.Entities.Student FromCreateEditToStudent (CreateEditStudentViewModel model, bool isNew, Guid imageId)
        {
            return new Data.Entities.Student
            {
                Id = isNew ? 0 : model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                ProfileImageId = imageId,
                SchoolClassId = model.SelectedSchoolClassId
            };
        }

        public CreateEditStudentViewModel ToCreateEditStudentViewModel(Data.Entities.Student student)
        {

            return new CreateEditStudentViewModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
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
    }
}
