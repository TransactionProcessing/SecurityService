namespace SecurityService.BusinessLogic.Requests{
    using System;
    using System.Collections.Generic;
    using MediatR;
    using Models;

    public class CreateUserRequest : IRequest<Unit>{
        #region Constructors

        private CreateUserRequest(Guid userId,
                                  String givenName,
                                  String middleName,
                                  String familyName,
                                  String userName,
                                  String password,
                                  String emailAddress,
                                  String phoneNumber,
                                  Dictionary<String, String> claims,
                                  List<String> roles){
            this.UserId = userId;
            this.GivenName = givenName;
            this.MiddleName = middleName;
            this.FamilyName = familyName;
            this.UserName = userName;
            this.Password = password;
            this.EmailAddress = emailAddress;
            this.PhoneNumber = phoneNumber;
            this.Claims = claims;
            this.Roles = roles;
        }

        #endregion

        #region Properties

        public Dictionary<String, String> Claims{ get; }
        public String EmailAddress{ get; }
        public String FamilyName{ get; }
        public String GivenName{ get; }
        public String MiddleName{ get; }
        public String Password{ get; }
        public String PhoneNumber{ get; }
        public List<String> Roles{ get; }
        public Guid UserId{ get; }
        public String UserName{ get; }

        #endregion

        #region Methods

        public static CreateUserRequest Create(Guid userId, String givenName, String middleName, String familyName, String userName, String password, String emailAddress, String phoneNumber, Dictionary<String, String> claims, List<String> roles){
            return new CreateUserRequest(userId, givenName, middleName, familyName, userName, password, emailAddress, phoneNumber, claims, roles);
        }

        #endregion
    }

    public class GetUserRequest : IRequest<UserDetails>{
        #region Constructors

        private GetUserRequest(Guid userId){
            this.UserId = userId;
        }

        #endregion

        #region Properties

        public Guid UserId{ get; }

        #endregion

        #region Methods

        public static GetUserRequest Create(Guid userId){
            return new GetUserRequest(userId);
        }

        #endregion
    }

    public class GetUsersRequest : IRequest<List<UserDetails>>{
        public String UserName{ get; }

        #region Fields

        #endregion

        #region Constructors

        private GetUsersRequest(String userName){
            this.UserName = userName;
        }

        #endregion

        #region Methods

        public static GetUsersRequest Create(String userName){
            return new GetUsersRequest(userName);
        }

        #endregion
    }
}