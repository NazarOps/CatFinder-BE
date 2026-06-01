using ApplicationLayer.Auth.DTOs;
using DomainLayer.Models.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<OperationResult<bool>>
    {
        public ForgotPasswordDTO forgotPasswordDTO { get; }

        public ForgotPasswordCommand(ForgotPasswordDTO _forgotPasswordDTO)
        {
            forgotPasswordDTO = _forgotPasswordDTO;
        }
    }
}
