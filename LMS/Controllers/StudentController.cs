using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var courseQuery = from temp_enrolled in db.Enrolleds
                              join temp_classes in db.Classes
                              on temp_enrolled.ClassNum equals temp_classes.ClassNum
                              into ec_pairs
                              from ec_pair in ec_pairs.DefaultIfEmpty()
                              join temp_courses in db.Courses
                              on ec_pair.CourseNum equals temp_courses.CourseNum
                              into ecc_pair
                              from ecc_final in ecc_pair.DefaultIfEmpty()
                              where temp_enrolled.UId == uid
                              select new
                              {
                                  subject = ecc_final.Subject,
                                  number = ecc_final.Num,
                                  name = ecc_final.Name,
                                  season = ec_pair.Semester,
                                  year = ec_pair.Year,
                                  grade = temp_enrolled.Grade
                              };
                              

            return Json(courseQuery.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            

            var getAssignmentQuery = from temp_course in db.Courses
                                     join temp_class in db.Classes
                                     on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                     into course_class
                                     from cc_pair in course_class.DefaultIfEmpty()
                                     join temp_assignCat in db.AssignmentCategories
                                     on new { X = cc_pair.ClassNum } equals new { X = temp_assignCat.ClassNum }
                                     into assigncat_class
                                     from cca_pair in assigncat_class.DefaultIfEmpty()
                                     join temp_assign in db.Assignments
                                     on new { Z = cca_pair.CategoryNum } equals new { Z = temp_assign.CategoryNum }
                                     into ccaa_pair
                                     from contentTable in ccaa_pair.DefaultIfEmpty()
                                     join temp_submission in db.Submissions
                                     on new { U = contentTable.AssignmentNum, S = uid } equals new { U = temp_submission.AssignmentNum, S = temp_submission.UId }
                                     into submission_class
                                     from final_table in submission_class.DefaultIfEmpty()
                                     where contentTable!= null && temp_course.Subject.Equals(subject) && temp_course.Num == num
                                      select new
                                      {
                                          aname = contentTable.Name,
                                          cname = cca_pair.Name,
                                          due = contentTable.DueDate,
                                          score = final_table == null? null : final_table.Score
                                      };
            return Json(getAssignmentQuery.ToArray());
            ///System.InvalidOperationException: 'Nullable object must have a value.'
            //working
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var submissionQuery = from temp_course in db.Courses
                                  join temp_class in db.Classes
                                  on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                  into course_class
                                  from cc_pair in course_class.DefaultIfEmpty()
                                  join temp_assignCat in db.AssignmentCategories
                                  on new { X = cc_pair.ClassNum, Y = category } equals new { X = temp_assignCat.ClassNum, Y = temp_assignCat.Name }
                                  into assigncat_class
                                  from cca_pair in assigncat_class.DefaultIfEmpty()
                                  join temp_assign in db.Assignments
                                  on new { Z = cca_pair.CategoryNum, W = asgname } equals new { Z = temp_assign.CategoryNum, W = temp_assign.Name }
                                  into ccaa_pair
                                  from contentTable in ccaa_pair.DefaultIfEmpty()
                                  where subject.Equals(temp_course.Subject) && num == temp_course.Num
                                  select contentTable.AssignmentNum;

            uint assignNum = submissionQuery.First();

            var submissionCheckQuery = from temp_submission in db.Submissions
                                       where temp_submission.AssignmentNum == assignNum
                                       && temp_submission.UId == uid
                                       select temp_submission;
            if(submissionCheckQuery.Count() > 0)
            {
                submissionCheckQuery.First().Contents = contents;
                submissionCheckQuery.First().Time = DateTime.Now;
            }
            else
            {
                Submission sub = new Submission();
                sub.UId = uid;
                sub.AssignmentNum = assignNum;
                sub.Score = 0;
                sub.Time = DateTime.Now;
                sub.Contents = contents;

                db.Submissions.Add(sub);
            }

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var classNumQuery = from temp_course in db.Courses
                                join temp_class in db.Classes
                                on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                into course_class
                                from cc_pair in course_class.DefaultIfEmpty()
                                where subject.Equals(temp_course.Subject) && num == temp_course.Num
                                select cc_pair.ClassNum;

            uint classNum = classNumQuery.First();

            Enrolled enroll = new Enrolled();

            enroll.UId = uid;
            enroll.ClassNum = classNum;
            enroll.Grade = "--";
            
            db.Enrolleds.Add(enroll);

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var gpaQuery = from temp_enrolled in db.Enrolleds
                           where temp_enrolled.UId == uid
                           select temp_enrolled.Grade;

            double calGPA = 0;
            double count = 0;
            if (gpaQuery.Count() == 0)
            {
                return Json(new {
                    gpa = 0.0
                });
            }
            foreach(string grade in gpaQuery)
            {
                count++;
                if (grade.Equals("A"))
                {
                    calGPA += 4.0;

                }else if(grade.Equals("A-"))
                {
                    calGPA += 3.7;
                }
                else if (grade.Equals("B+"))
                {
                    calGPA += 3.3;
                }
                else if (grade.Equals("B"))
                {
                    calGPA += 3.0;
                }
                else if (grade.Equals("B-"))
                {
                    calGPA += 2.7;
                }
                else if (grade.Equals("C+"))
                {
                    calGPA += 2.3;
                }
                else if (grade.Equals("C"))
                {
                    calGPA += 2.0;
                }
                else if (grade.Equals("C-"))
                {
                    calGPA += 1.7;
                }
                else if (grade.Equals("D+"))
                {
                    calGPA += 1.3;
                }
                else if (grade.Equals("D"))
                {
                    calGPA += 1.0;
                }
                else if (grade.Equals("D-"))
                {
                    calGPA += 0.7;
                }
                else if (grade.Equals("E"))
                {
                    calGPA += 0.0;
                }else if (grade.Equals("--"))
                {
                    count--;
                }
            }

            if (count !=0) calGPA /= count;

            return Json(new
            {
                gpa = calGPA
            });
        }
                
        /*******End code to modify********/

    }
}

