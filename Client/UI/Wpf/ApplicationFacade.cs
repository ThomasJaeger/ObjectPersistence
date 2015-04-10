using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using DTO;
using Services;

namespace Wpf
{
    public class ApplicationFacade
    {
        private static readonly ApplicationFacade _instance = new ApplicationFacade();

        private ApplicationFacade()
        {
            // Do any initialization here
        }

        public static string ErrorMessage { get; set; }

        public static List<PersonDTO> GetAllPeople()
        {
            // Do any client side logging and/or validations here
            return RESTServices.GetAllPeople();
        }

        public static bool CreatePerson(PersonDTO dto)
        {
            // Do any client side logging and/or validations here

//            if (string.IsNullOrEmpty(dto.FirstName) || string.IsNullOrEmpty(dto.LastName) || string.IsNullOrEmpty(dto.Email))
//            {
//                MessageBox.Show("First name, last name, and email can not be empty.");
//                return false;
//            }

            RESTServices.CreatePerson(dto);

            if (RESTServices.ResultType == ResultType.Success)
            {
                MessageBox.Show(dto.FirstName + " " + dto.LastName + " was created successfully.");
                return true;
            }

            HandleExceptions();
            return false;
        }

        private static void HandleExceptions()
        {
            string validations = string.Join(Environment.NewLine, (object[])RESTServices.ErrorInfo.ErrorDatas.ToArray());

            MessageBox.Show("Error code: " + RESTServices.ErrorInfo.Code + Environment.NewLine +
                "Error severity type: " + RESTServices.ErrorInfo.ErrorSeverityType + Environment.NewLine +
                "Error message: " + RESTServices.ErrorInfo.Message + Environment.NewLine +
                "Error description: " + RESTServices.ErrorInfo.Description + Environment.NewLine +
                "Validation errors: " + Environment.NewLine + validations);
        }

        public static bool SavePerson(PersonDTO dto)
        {
            if (dto == null)
            {
                MessageBox.Show("PersonDto can not be null.");
                return false;
            }

            RESTServices.SavePerson(dto);

            if (RESTServices.ResultType == ResultType.Success)
            {
                MessageBox.Show(dto.FirstName + " " + dto.LastName + " was updated successfully.");
                return true;
            }

            HandleExceptions();
            return false;
        }

        public static bool DeletePerson(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("id can not be null.");
                return false;
            }

            RESTServices.DeletePerson(id);

            if (RESTServices.ResultType == ResultType.Success)
                return true;

            HandleExceptions();
            return false;
        }

        public static List<AddressTypeDTO> GetAllAddressTypes()
        {
            // Do any client side logging and/or validations here
            return RESTServices.GetAllAddressTypes();
        }
    }
}
