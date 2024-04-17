/**
 * @file IModel.cs
 *
 * @brief Defines the interface for models in the business logic layer.
 *
 * This file contains the definition of the IModel interface, which serves as the base interface for models in the business logic layer (BL). Models implementing this interface are expected to provide an identifier property, as dictated by the IWithId interface.
 *
 * @see IWithId
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common;

namespace BL.Models.Interfaces;

/// <summary>
///     Represents an interface for a generic model.
/// </summary>
public interface IModel : IWithId
{
}
